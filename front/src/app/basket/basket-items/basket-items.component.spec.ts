import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BasketItemsComponent } from './basket-items.component';

describe('BasketItemsComponent', () => {
  let component: BasketItemsComponent;
  let fixture: ComponentFixture<BasketItemsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BasketItemsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BasketItemsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
