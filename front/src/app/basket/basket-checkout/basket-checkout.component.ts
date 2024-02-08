import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService, SelectItem } from 'primeng/api';
import { Basket } from '../shared/basket.model';
import { BasketService } from '../shared/basket.service';
import {
  IPayPalConfig,
  ICreateOrderRequest
} from 'ngx-paypal';
import { OrdersService } from 'src/app/orders/shared/orders.service';
import { CancelOrderDto, CaptureOrderDto } from 'src/app/orders/shared/order.model';
export type SelectFormControl = FormControl & { value: string };

@Component({
  selector: 'app-basket-checkout',
  templateUrl: './basket-checkout.component.html',
  styleUrls: ['./basket-checkout.component.scss'],
  providers: [MessageService]
})
export class BasketCheckoutComponent implements OnInit {

  total: number = 0;
  paymentMethods: SelectItem<string>[];
  selectedPaymentMethod: string;
  isSending = false;
  public payPalConfig?: IPayPalConfig;

  checkoutForm = new FormGroup({
    address: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(70)]),
    comment: new FormControl('', [Validators.minLength(1), Validators.maxLength(100)]),
    paymentMethod: new FormControl({ label: 'Cash on Delivery', value: 'caseondelivery' }) as SelectFormControl,
  });

  constructor(private basketService: BasketService, private orderService: OrdersService, private messageService: MessageService, private router: Router) {

  }

  ngOnInit(): void {
    this.initConfig();
    this.basketService.totalObservable.subscribe(
      data => {
        this.total = data;
      }
    )
    this.paymentMethods = [
      { label: 'Cash on Delivery', value: 'caseondelivery' },
      { label: 'PayPal', value: 'paypal' }
    ];
  }

  checkout() {
    // console.log(this.checkoutForm.value);
    let checkout = {
      address: this.checkoutForm.value.address,
      comment: this.checkoutForm.value.comment,
      paymentMethod: this.checkoutForm.value.paymentMethod.value,
    }
    this.isSending = true;
    if (this.checkoutForm.valid) {
      this.basketService.checkout(checkout).subscribe(
        data => {
          this.isSending = false;
          // erace previous basket
          this.basketService.setBasket(new Basket()).subscribe(data => { });
          this.basketService.totalObservable.next(0);
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order successfully created' });
          this.router.navigateByUrl("/orders");
        },
        (error) => {
          this.isSending = false;
          // show error
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error.message });
        }
      )
    } else {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: "Please check all input fields" });
    }
  }



  private initConfig(): void {
    this.payPalConfig = {
      clientId: 'AUVle8qulaOdCGWzbEy9trmYmo9brhbkMzOWdt5Z0wFPxcLLfYs-2rTMUAbmquEpt7UDGzqjlbfgZkq8',
      // for creating orders (transactions) on server see
      // https://developer.paypal.com/docs/checkout/reference/server-integration/set-up-transaction/
      createOrderOnServer: (data) => this.basketService.checkout({
        address: this.checkoutForm.value.address,
        comment: this.checkoutForm.value.comment,
        paymentMethod: "paypal",
      }).toPromise()
        .then((res) => res.orderId),
      onApprove: (data, actions) => {
        console.log('onApprove - transaction was approved, but not authorized', data, actions);
        actions.order.get().then(details => {
          console.log('onApprove - you can get full order details inside onApprove: ', details);
        });

      },
      onClientAuthorization: (data) => {
        console.log('onClientAuthorization - you should probably inform your server about completed transaction at this point', data);
        // this.showSuccess = true;
        this.orderService.captureOrder(new CaptureOrderDto(data.id)).subscribe(
          data => {
            this.basketService.totalObservable.next(0);
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order successfully canceled' });
            this.router.navigateByUrl("/orders");
          },
          (error) => {
            // show error
            this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error.message });
          }
        )
      },
      onCancel: (data, actions) => {
        console.log('OnCancel', data, actions);
        // this.showCancel = true;
        this.orderService.cancelOrder(new CancelOrderDto(data.orderID)).subscribe(
          data => {
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order successfully canceled' });
            this.router.navigateByUrl("/orders");
          },
          (error) => {
            // show error
            this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error.message });
          }
        )
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
