import { Component, Input, OnInit } from '@angular/core';
import { Product } from 'src/app/products/shared/product.model';
import { ProductService } from 'src/app/products/shared/product.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-basket-item-product',
  templateUrl: './basket-item-product.component.html',
  styleUrls: ['./basket-item-product.component.scss']
})
export class BasketItemProductComponent implements OnInit {

  product: Product
  imageUrl;
  @Input() set productId(value: number) {

    this.imageUrl = environment.serverUrl + '/product/' + value + "/image";
    this.productService.getProduct(value).subscribe(
      data => {
        this.product = data;
      }
    )
  }

  constructor(private productService: ProductService) {

  }

  ngOnInit(): void {
  }

}
