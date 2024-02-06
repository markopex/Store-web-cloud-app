import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Basket } from '../shared/basket.model';
import { BasketService } from '../shared/basket.service';

@Component({
  selector: 'app-basket-checkout',
  templateUrl: './basket-checkout.component.html',
  styleUrls: ['./basket-checkout.component.scss'],
  providers: [MessageService]
})
export class BasketCheckoutComponent implements OnInit {

  total: number = 0;

  isSending = false;

  checkoutForm = new FormGroup({
    address: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(70)]),
    comment: new FormControl('', [ Validators.minLength(1), Validators.maxLength(100)]),
  });

  constructor(private basketService: BasketService, private messageService: MessageService, private router: Router) { }

  ngOnInit(): void {
    this.basketService.totalObservable.subscribe(
      data => {
        this.total = data;
      }
    )
  }

  checkout(){
    this.isSending = true;
    this.basketService.checkout(this.checkoutForm.value).subscribe(
      data => {
        this.isSending = false;
        // erace previous basket
        this.basketService.setBasket(new Basket()).subscribe(data => {});
        this.basketService.totalObservable.next(0);
        this.messageService.add({severity:'success', summary: 'Success', detail: 'Order successfully created'});
        this.router.navigateByUrl("/orders");
      },
      (error) =>{
        this.isSending = false;
        // show error
        this.messageService.add({severity:'error', summary: 'Error', detail: error.error.message});
      }
    )
  }

}
