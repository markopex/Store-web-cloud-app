import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { CancelOrderDto, CaptureOrderDto, Order } from './order.model';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {

  constructor(private http: HttpClient) { }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(environment.serverUrl + "/orders");
  }
  captureOrder(captureOrderDto: CaptureOrderDto): Observable<Object> {
    return this.http.post<Object>(environment.serverUrl + "/orders/capture", captureOrderDto);
  }
  cancelOrder(dto: CancelOrderDto): Observable<Object> {
    return this.http.post<Object>(environment.serverUrl + "/orders/cancel", dto);
  }
}
