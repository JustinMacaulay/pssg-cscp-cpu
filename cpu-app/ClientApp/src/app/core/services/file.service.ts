import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { retry, catchError } from 'rxjs/operators';
import { iDynamicsPostFile, iDynamicsPostSignedContract } from '../models/dynamics-post';
import { iDynamicsFile } from '../models/dynamics-blob';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  baseUrl = environment.apiRootUrl;
  apiPath = this.baseUrl.concat('api/DynamicsFile');

  constructor(
    private http: HttpClient,
  ) { }


  getContractPackage(organizationId: string, userId: string, taskId: string): Observable<iDynamicsFile> {
    return this.http.get<iDynamicsFile>(`${this.apiPath}/contract_package/${organizationId}/${userId}/${taskId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  getContractDocuments(organizationId: string, userId: string, contractId: string): Observable<iDynamicsFile> {
    return this.http.get<iDynamicsFile>(`${this.apiPath}/${organizationId}/${userId}/documents/contract/${contractId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  getAccountDocuments(organizationId: string, userId: string, accountId: string): Observable<iDynamicsFile> {
    return this.http.get<iDynamicsFile>(`${this.apiPath}/${organizationId}/${userId}/documents/account/${accountId}`, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  uploadSignedContract(signedContract: iDynamicsPostSignedContract, taskId: string): Observable<any> {
    // may need to add the contract id into this postback
    return this.http.post<any>(`${this.apiPath}/signed_contract/${taskId}`, signedContract, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  uploadAccountDocuments(file: iDynamicsPostFile, accountId: string): Observable<any> {
    // console.log(this);
    // may need to add the contract id into this postback
    return this.http.post<any>(`${this.apiPath}/account/${accountId}`, file, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }
  uploadContractDocuments(file: iDynamicsPostFile, contractId: string): Observable<any> {
    // may need to add the contract id into this postback
    return this.http.post<any>(`${this.apiPath}/contract/${contractId}`, file, { headers: this.headers }).pipe(
      retry(3),
      catchError(this.handleError)
    );
  }

  downloadDocument(organizationId: string, userId: string, docId: string): Observable<iDynamicsFile> {
    return this.http.get<iDynamicsFile>(`${this.apiPath}/${organizationId}/${userId}/document/${docId}`, { headers: this.headers }).pipe(
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
