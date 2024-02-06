import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Product } from './product.model';
import { UpdateCreateProductDto } from './create-product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  constructor(private http: HttpClient) { }

  addProduct(product: FormData): Observable<Object> {
    return this.http.post<Product>(environment.serverUrl + '/product', product);
  }

  updateProduct(id: number, product: FormData): Observable<Product> {
    return this.http.put<Product>(environment.serverUrl + '/product/' + id, product);
  }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(environment.serverUrl + '/product');
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(environment.serverUrl + '/product/' + id);
  }
  deleteProduct(id: number): Observable<Object> {
    return this.http.delete<Object>(environment.serverUrl + '/product/' + id);
  }
}
