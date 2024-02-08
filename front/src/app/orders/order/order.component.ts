import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CountdownComponent } from 'ngx-countdown';
import { MessageService } from 'primeng/api';
import { Subscription, timer } from 'rxjs';
import { EventService } from 'src/app/Shared/event.service';
import { AuthService } from 'src/app/Shared/services/auth.service';
import { CancelOrderDto, CaptureOrderDto, Order } from '../shared/order.model';
import { OrdersService } from '../shared/orders.service';
import { IPayPalConfig } from 'ngx-paypal';

@Component({
  selector: 'app-order',
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.scss'],
  providers: [MessageService]
})
export class OrderComponent implements OnInit {

  subscription: Subscription;
  resolve = false
  public payPalConfig?: IPayPalConfig;

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
    this.paypalOrderId = order.paypalOrderId;
    this.orderedDate = new Date(order.utcTimeOrderCreated);
    if (order.paymentMethod == 0) {
      this.paymentMethod = "Cash on delivery";
    } else if (order.paymentMethod == 1) {
      this.paymentMethod = "PayPal";
    } else {
      this.paymentMethod = "Unknown";
    }
  }
  deliveredDate?: Date;
  orderedDate: Date;
  delivered = false;
  order: Order;
  isTaking = false;
  secondsUntil = -1;
  paymentMethod: string;
  paypalOrderId: string;

  constructor(private eventService: EventService, private orderService: OrdersService, private messageService: MessageService, private router: Router, private authService: AuthService) {
    //this.countdown.begin();

  }

  ngOnInit(): void {
  }

  cancelOrder() {
    this.orderService.cancelOrder(new CancelOrderDto(this.paypalOrderId)).subscribe(
      data => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order successfully canceled' });
        window.location.reload();
      },
      (error) => {
        // show error
        this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error.message });
      }
    )
  }

  payOrder() {
    // Implement logic to initiate payment...
    this.initConfig();
    this.resolve = true;
  }
  paypalOrderIdPromise = new Promise<string>((resolve, reject) => {
    console.log(this.paypalOrderId);
    console.log("usao " + this.order.paypalOrderId);
    // Assuming this.order.paypalOrderId is accessible in this context
    if (this.order.paypalOrderId) {
      resolve(this.order.paypalOrderId);
    } else {
      reject(new Error('PayPal order ID not found'));
    }
  });

  private initConfig(): void {
    this.payPalConfig = {
      clientId: 'AUVle8qulaOdCGWzbEy9trmYmo9brhbkMzOWdt5Z0wFPxcLLfYs-2rTMUAbmquEpt7UDGzqjlbfgZkq8',
      // for creating orders (transactions) on server see
      // https://developer.paypal.com/docs/checkout/reference/server-integration/set-up-transaction/
      createOrderOnServer: data => new Promise<string>((resolve, reject) => {
        console.log(this.paypalOrderId);
        console.log("usao " + this.order.paypalOrderId);
        // Assuming this.order.paypalOrderId is accessible in this context
        if (this.order.paypalOrderId) {
          console.log("nasao");
          resolve(this.order.paypalOrderId);
        } else {
          reject(new Error('PayPal order ID not found'));
        }
      })
      ,//i.find(i => i.id == this.order.id).payPalOrderId
      onClientAuthorization: (data) => {
        console.log('onClientAuthorization - you should probably inform your server about completed transaction at this point', data);
        // this.showSuccess = true;
        this.orderService.captureOrder(new CaptureOrderDto(data.id)).subscribe(
          data => {
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order successfully canceled' });
            // this.router.navigateByUrl("/orders");
            window.location.reload();
          },
          (error) => {
            // show error 
            this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error.message });
          }
        )
      },
      onCancel: (data, actions) => {
        console.log('OnCancel', data, actions);
        this.resolve = false;
      },
      onError: err => {
        console.log('OnError', err);
        // this.showError = true;
      },
      onClick: (data, actions) => {
        console.log('onClick', data, actions);
        // this.resetStatus();
      },
    };
  }
}