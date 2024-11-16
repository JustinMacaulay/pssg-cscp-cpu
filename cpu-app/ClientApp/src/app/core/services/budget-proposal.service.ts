import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { retry, catchError } from 'rxjs/operators';
import { iDynamicsBudgetProposal } from '../models/dynamics-blob';
import { iDynamicsPostBudgetProposal } from '../models/dynamics-post';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BudgetProposalService {
  baseUrl = environment.apiRootUrl;
  apiPath = this.baseUrl.concat('api/DynamicsBudgetProposal');

  constructor(
    private http: HttpClient,
  ) { }

  getBudgetProposal(organizationId: string, userId: string, contractId: string): Observable<iDynamicsBudgetProposal> {
    return this.http.get<iDynamicsBudgetProposal>(`${this.apiPath}/${organizationId}/${userId}/${contractId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  setBudgetProposal(budgetProposal: iDynamicsPostBudgetProposal): Observable<any> {
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
