import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ProductService } from 'src/app/products/shared/product.service';
import { BasketItem } from '../shared/basket-item.model';
import { Basket } from '../shared/basket.model';
import { BasketService } from '../shared/basket.service';

@Component({
  selector: 'app-basket-items',
  templateUrl: './basket-items.component.html',
  styleUrls: ['./basket-items.component.scss'],
  providers: [MessageService]
})
export class BasketItemsComponent implements OnInit {

  @Input()
  basket: Basket;
  isUpdating = false;

  constructor(private basketService: BasketService, private messageService: MessageService, private router: Router, private productService: ProductService) {
    
   }

  ngOnInit(): void {
  }

  removeItem(item: BasketItem){
    const index = this.basket.basketItems.indexOf(item);
    if (index > -1) { // only splice array when item is found
      this.basket.basketItems.splice(index, 1); // 2nd parameter means remove one item only
    }
    this.updateTotal();
  }

  update(){
    this.isUpdating = true;
    this.basketService.setBasket(this.basket).subscribe(
      data => {
        this.messageService.add({severity:'success', summary: 'Success', detail: 'Basket successfully updated.'});
        this.isUpdating = false;
        this.updateTotal();
      },
      error => {
        this.isUpdating = false;
        this.messageService.add({severity:'error', summary: 'Error', detail: error.error.message});
      }
    )
  }

  updateTotal(){
    let sum = 0;
    this.basket.basketItems.forEach(i => {
      sum += i.product?.price * i.quantity 
    })
    this.basketService.totalObservable.next(sum);
  }

}
