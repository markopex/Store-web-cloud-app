import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BasketItemProductComponent } from './basket-item-product.component';

describe('BasketItemProductComponent', () => {
  let component: BasketItemProductComponent;
  let fixture: ComponentFixture<BasketItemProductComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BasketItemProductComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BasketItemProductComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
