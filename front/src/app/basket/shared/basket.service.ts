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

  constructor( private http: HttpClient) { }

  public totalObservable = new BehaviorSubject<number>(0);

  setBasket(basket: Basket) :Observable<Object> {
    return this.http.post<Object>(environment.serverUrl + '/basket', basket);
  }

  getBasket(): Observable<Basket>{
    return this.http.get<Basket>(environment.serverUrl+ '/basket');
  }

  checkout(checkout: Object): Observable<Object>{
    return this.http.post<Object>(environment.serverUrl + '/basket/checkout', checkout);
  }

  addToBasket(item: BasketItem) :Observable<Object> {
    return this.http.put<Object>(environment.serverUrl + '/basket', item);
  }
}
