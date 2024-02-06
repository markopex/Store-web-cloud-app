import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from 'src/app/products/shared/product.service';
import { Basket } from '../shared/basket.model';
import { BasketService } from '../shared/basket.service';

@Component({
  selector: 'app-basket',
  templateUrl: './basket.component.html',
  styleUrls: ['./basket.component.scss']
})
export class BasketComponent implements OnInit {

  basket: Basket;
  total: number = 0;

  constructor(private basketService: BasketService, private router: Router, private productService: ProductService) { 
    this.basketService.getBasket().subscribe(
      data => {
        data.basketItems.forEach(item => {
          this.productService.getProduct(item.productId).subscribe(
            product => {
              item.product = product;
              this.total += product.price * item.quantity;
              this.updateTotal();
            }
          );
        });
        this.basket = data;
        console.log(data);
      }
    )
  }

  ngOnInit(): void {
  }

  

  updateTotal(){
    this.basketService.totalObservable.next(this.total);
  }

}
