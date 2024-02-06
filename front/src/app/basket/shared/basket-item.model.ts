import { Product } from "src/app/products/shared/product.model";

export class BasketItem{
    productId: number = 0;
    quantity: number = 0;
    product?: Product = null;
}