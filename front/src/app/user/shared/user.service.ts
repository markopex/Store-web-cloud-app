import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Token } from './token.model';
import { UpdateSellerStatus } from './update-seller-status.model';
import { User } from './user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) { }

  login(login: Object): Observable<Token> {
    return this.http.post<Token>(environment.serverUrl + '/login', login);
  }
  googleLogin(login: Object): Observable<Token> {
    return this.http.post<Token>(environment.serverUrl + '/login-google', login);
  }
  register(registration: Object): Observable<Object> {
    return this.http.post<Object>(environment.serverUrl + '/register', registration);
  }

  updateUser(update: Object): Observable<Object> {
    return this.http.put<Object>(environment.serverUrl + '/users', update);
  }

  getSellers(): Observable<User[]> {
    return this.http.get<User[]>(environment.serverUrl + '/users/sellers');
  }

  getUser(): Observable<User> {
    return this.http.get<User>(environment.serverUrl + "/users");
  }

  apply(): Observable<Object> {
    return this.http.post<Object>(environment.serverUrl + "/users/sellers", new Object());
  }

  approveSeller(seller: string): Observable<Object> {
    let updateSellerStatus = new UpdateSellerStatus();
    updateSellerStatus.user = seller;
    return this.http.put<Object>(environment.serverUrl + "/users/sellers", updateSellerStatus);
  }

  denySeller(seller: string): Observable<Object> {
    let updateSellerStatus = new UpdateSellerStatus();
    updateSellerStatus.user = seller;
    updateSellerStatus.approved = false;
    return this.http.put<Object>(environment.serverUrl + "/users/sellers", updateSellerStatus);
  }
  uploadImage(image: FormData): Observable<Object> {
    return this.http.post<Object>(environment.serverUrl + "/users/image", image);
  }
  getImage(): Observable<Blob> {
    return this.http.get<Blob>(environment.serverUrl + "/users/image");
  }

  downloadFile(): Observable<HttpEvent<Blob>> {
    return this.http.request(new HttpRequest(
      'GET',
      environment.serverUrl + "/users/image/me",
      null,
      {
        reportProgress: true,
        responseType: 'blob'
      }));
  }

  downloadImage(email: string): Observable<HttpEvent<Blob>> {
    return this.http.request(new HttpRequest(
      'GET',
      environment.serverUrl + "/users/image?email=" + email,
      null,
      {
        reportProgress: true,
        responseType: 'blob'
      }));
  }
}
