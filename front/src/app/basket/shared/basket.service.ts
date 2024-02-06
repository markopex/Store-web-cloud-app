import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { BasketItem } from './basket-item.model';
import { Basket } from './basket.model';
import { Checkout } from './checkout.model';

@Injectable({
  providedIn: 'root'
})
export class BasketService {

  serverUrl: string = "http://localhost:8080/api";//environment.serverUrl;
  constructor(private http: HttpClient) { }

  public totalObservable = new BehaviorSubject<number>(0);

  setBasket(basket: Basket): Observable<Object> {
    return this.http.post<Object>(this.serverUrl + '/basket', basket);
  }

  getBasket(): Observable<Basket> {
    return this.http.get<Basket>(this.serverUrl + '/basket');
  }

  checkout(checkout: Object): Observable<Object> {
    return this.http.post<Object>(this.serverUrl + '/basket/checkout', checkout);
  }

  addToBasket(item: BasketItem): Observable<Object> {
    return this.http.put<Object>(this.serverUrl + '/basket', item);
  }
}
