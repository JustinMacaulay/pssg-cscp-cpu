import { retry, catchError } from 'rxjs/operators';
import { iDynamicsProgramSurplusResponse, iDynamicsScheduleFResponse } from '../models/dynamics-blob';
import { iDynamicsPostScheduleF } from '../models/dynamics-post';
import { Observable, throwError, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class ProgramSurplusService {
  baseUrl = environment.apiRootUrl;
  apiPath = this.baseUrl.concat('api/DynamicsProgramSurplus');

  constructor(
    private http: HttpClient,
  ) { }

  getProgramSurplus(organizationId: string, userId: string, scheduleFId: string): Observable<iDynamicsProgramSurplusResponse> {
    return this.http.get<iDynamicsProgramSurplusResponse>(`${this.apiPath}/${organizationId}/${userId}/${scheduleFId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  setProgramSurplus(budgetProposal: iDynamicsPostScheduleF): Observable<any> {
    return this.http.post<any>(`${this.apiPath}`, budgetProposal, { headers: this.headers }).pipe(
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
