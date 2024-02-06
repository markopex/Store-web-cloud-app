import { Component, OnInit } from '@angular/core';
import { EventService } from 'src/app/Shared/event.service';
import { AuthService } from 'src/app/Shared/services/auth.service';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnInit {
  

  role: string = "";

  constructor(private authService: AuthService) {
    this.authService.roleStateObservable.subscribe(
      data => {
        this.role = data;
      }
    )
   }

  ngOnInit(): void {
    
  }

}
