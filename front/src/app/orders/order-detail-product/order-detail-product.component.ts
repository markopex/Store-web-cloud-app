import { Component, Input, OnInit } from '@angular/core';
import { OrderItem } from '../shared/order-item.model';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-order-detail-product',
  templateUrl: './order-detail-product.component.html',
  styleUrls: ['./order-detail-product.component.scss']
})
export class OrderDetailProductComponent implements OnInit {
  currentOrderItem: OrderItem;
  imageUrl;
  @Input() set orderItem(orderItem: OrderItem) {
    this.currentOrderItem = orderItem;
    this.imageUrl = environment.serverUrl + '/product/' + orderItem.productId + "/image";
  }

  constructor() { }

  ngOnInit(): void {
  }

}
