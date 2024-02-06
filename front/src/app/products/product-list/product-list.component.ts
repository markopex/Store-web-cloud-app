import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { AuthService } from 'src/app/Shared/services/auth.service';
import { Product } from '../shared/product.model';
import { ProductService } from '../shared/product.service';
import { UpdateCreateProductDto } from '../shared/create-product.model';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
  providers: [MessageService]
})
export class ProductListComponent implements OnInit {

  products: Product[] = [];

  role = this.authService.roleStateObservable.value;

  displayModal: boolean;
  productFile: any;
  addForm = new FormGroup({
    id: new FormControl(0),
    name: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(50)]),
    description: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(500)]),
    price: new FormControl(0, [Validators.required, Validators.min(1)]),
    quantity: new FormControl(0, [Validators.required, Validators.min(0)]),
  });
  isAdding = false;
  isEditing = false;
  constructor(private productService: ProductService, private messageService: MessageService, private authService: AuthService) {
    this.loadProducts();
  }

  ngOnInit(): void {
    this.authService.roleStateObservable.subscribe(
      data => {
        this.role = data;
      }
    )
  }

  loadProducts() {
    this.productService.getProducts().subscribe(
      data => {
        this.products = data;
        console.log(this.products);
      },
      error => {

      }
    )
  }

  showAddDialog() {
    this.addForm.setValue(
      {
        'id': null,
        'description': "",
        'name': "",
        'price': 1,
        'quantity': 1,
      }
    );
    this.isEditing = false;
    this.displayModal = true;
  }
  onSelectImage(event) {
    this.productFile = event.currentFiles[0];
  }

  addProduct() {
    if (this.addForm.valid) {
      this.isAdding = true;
      let createProductDto = this.addForm.value as UpdateCreateProductDto
      let formData = new FormData();
      formData.append("Name", createProductDto.name);
      formData.append("Description", createProductDto.description);
      formData.append("Price", createProductDto.price.toString());
      formData.append("Quantity", createProductDto.quantity.toString());
      console.log(this.productFile);
      if (this.productFile === undefined) {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: "Please set image" });
        this.isAdding = false;
        return;
      }
      formData.append('ImageFile', this.productFile, this.productFile?.name);
      this.productService.addProduct(formData).subscribe(
        data => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Product added successfully' });
          this.displayModal = false;
          this.loadProducts();
          this.isAdding = false;
          this.productFile = null;
        },
        error => {
          this.isAdding = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.error.message });
        }
      )
    }
  }

  updateProduct() {
    if (this.addForm.valid) {
      this.isAdding = true;
      let createProductDto = this.addForm.value as UpdateCreateProductDto
      let formData = new FormData();
      let id = this.addForm.value['id'];
      if (id == null) return;
      formData.append("Name", createProductDto.name);
      formData.append("Description", createProductDto.description);
      formData.append("Price", createProductDto.price.toString());
      formData.append("Quantity", createProductDto.quantity.toString());
      console.log(this.productFile);
      if (this.productFile !== undefined) {
        formData.append('ImageFile', this.productFile!, this.productFile?.name);
      }
      this.productService.updateProduct(id, formData).subscribe(
        data => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Product updated successfully' });
          this.displayModal = false;
          this.loadProducts();
          this.isAdding = false;
          this.productFile = null;
        },
        error => {
          this.isAdding = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: "Unable to save changes" });
        }
      )
    }
  }

  onProductDeleted(productId: number) {
    this.deleteProductFromList(productId);
  }

  deleteProductFromList(productId: number) {
    const index: number = this.products.findIndex(i => i.id == productId);
    if (index !== -1) {
      this.products.splice(index, 1);
    }
  }

  onProductEdit(product: Product) {
    console.log(product);
    this.isEditing = true;
    this.addForm.setValue(
      {
        'id': product.id,
        'description': product.description,
        'name': product.name,
        'price': product.price,
        'quantity': product.quantity,
      }
    )
    this.isAdding = false;
    this.displayModal = true;
  }

}
