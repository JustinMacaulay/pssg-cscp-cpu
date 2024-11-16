import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { retry, catchError } from 'rxjs/operators';
import { iDynamicsMonthlyStatisticsQuestions } from '../models/dynamics-blob';
import { iDynamicsPostStatusReport } from '../models/dynamics-post';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StatusReportService {
  // This service does a "collect questions" and a "set and forget".
  // It is unlike other parts of the application in that the user is expected not to stop and resume the process. Type them all in at once and send it.

  baseUrl = environment.apiRootUrl;
  apiPath = this.baseUrl.concat('api/DynamicsStatusReport');

  constructor(
    private http: HttpClient,
  ) { }

  getStatusReportQuestions(organizationId: string, userId: string, taskId: string): Observable<iDynamicsMonthlyStatisticsQuestions> {
    return this.http.get<iDynamicsMonthlyStatisticsQuestions>(`${this.apiPath}/${organizationId}/${userId}/${taskId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  setStatusReportAnswers(taskId: string, answers: iDynamicsPostStatusReport): Observable<any> {
    return this.http.post<any>(`${this.apiPath}/${taskId}`, answers, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  getStatusReportAnswers(organizationId: string, userId: string, dataCollectionId: string): Observable<iDynamicsMonthlyStatisticsQuestions> {
    return this.http.get<iDynamicsMonthlyStatisticsQuestions>(`${this.apiPath}/data_collection/${organizationId}/${userId}/${dataCollectionId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  getMonthlyStats(organizationId: string, userId: string, contractId: string) {
    return this.http.get<iDynamicsMonthlyStatisticsQuestions>(`${this.apiPath}/monthly_stats/${organizationId}/${userId}/${contractId}`, { headers: this.headers }).pipe(
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
