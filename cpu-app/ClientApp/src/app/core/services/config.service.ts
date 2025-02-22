import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Configuration } from '../models/configuration.interface';

@Injectable({
  providedIn: 'root'
})
export class ConfigService{
  baseUrl = environment.apiRootUrl;
  apiPath = this.baseUrl.concat('api/Configuration');
  headers: HttpHeaders = new HttpHeaders({
    'Content-Type': 'application/json'
  });

  constructor(
    private http: HttpClient,
  ) { }

  public async load(): Promise<Configuration> {
    try {
      console.log(this.baseUrl)
      return await
        this.http.get<Configuration>(this.apiPath, { headers: this.headers })
          .pipe(catchError(this.handleError)).toPromise();
    } catch (error) {
      this.handleError(error);
      throw error;
    }
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
