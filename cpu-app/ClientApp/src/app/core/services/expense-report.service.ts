import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { retry, catchError } from 'rxjs/operators';
import { iDynamicsScheduleGResponse } from '../models/dynamics-blob';
import { iDynamicsPostScheduleG } from '../models/dynamics-post';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ExpenseReportService {
  baseUrl = environment.apiRootUrl;
  apiPath = this.baseUrl.concat('api/DynamicsExpenseReport');

  constructor(
    private http: HttpClient,
  ) { }

  getExpenseReport(organizationId: string, userId: string, expenseReportId: string): Observable<iDynamicsScheduleGResponse> {
    return this.http.get<iDynamicsScheduleGResponse>(`${this.apiPath}/${organizationId}/${userId}/${expenseReportId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  setExpenseReport(scheduleG: iDynamicsPostScheduleG): Observable<any> {
    return this.http.post<any>(`${this.apiPath}`, scheduleG, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }

  get headers(): HttpHeaders {
    return new HttpHeaders({ 'Content-Type': 'application/json' });
  }
  protected handleError(err): Observable<never> {
    let errorMessage = '';
    if (err.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      errorMessage = err.error.message;
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong,
      errorMessage = `Backend returned code ${err.status}, body was: ${err.message}`;
    }
    return throwError(errorMessage);
  }
}
