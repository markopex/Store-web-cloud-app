import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CountdownComponent } from 'ngx-countdown';
import { MessageService } from 'primeng/api';
import { Subscription, timer } from 'rxjs';
import { EventService } from 'src/app/Shared/event.service';
import { AuthService } from 'src/app/Shared/services/auth.service';
import { Order } from '../shared/order.model';
import { OrdersService } from '../shared/orders.service';

@Component({
  selector: 'app-order',
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.scss'],
  providers: [MessageService]
})
export class OrderComponent implements OnInit {

  subscription: Subscription;


  startTimer(duration: number) {
    const source = timer(0, 1000);
    this.subscription = source.subscribe(val => {
      this.secondsUntil -= 1;
      if (this.secondsUntil == 0) {
        this.stopTimer();
        //this.eventService.refresh();
        //window.location.reload();
      }
    });
  }

  stopTimer() {
    this.subscription.unsubscribe();
  }

  role = this.authService.roleStateObservable.value;

  @Input() set ord(order: Order) {
    this.order = order;
    let currentTime = new Date().getTime();
    this.orderedDate = new Date(order.utcTimeOrderCreated);
    if (order.utcTimeDeliveryExpected != 0)
      this.deliveredDate = new Date(order.utcTimeDeliveryExpected);

    if (this.order.utcTimeDeliveryExpected > currentTime) {
      this.secondsUntil = Math.floor((this.order.utcTimeDeliveryExpected - currentTime) / 1000);
      this.startTimer(this.secondsUntil);
    } else {
      if (this.order.utcTimeDeliveryExpected != 0) {
        this.secondsUntil = 0;
      }
    }
  }
  deliveredDate?: Date;
  orderedDate: Date;
  delivered = false;
  order: Order;
  isTaking = false;
  secondsUntil = -1;

  constructor(private eventService: EventService, private ordersService: OrdersService, private messageService: MessageService, private router: Router, private authService: AuthService) {
    //this.countdown.begin();

  }

  ngOnInit(): void {
  }

  cancelOrder() { }
}
