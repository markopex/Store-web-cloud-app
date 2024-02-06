import { Component } from '@angular/core';
import { MessageService, PrimeNGConfig } from 'primeng/api';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [MessageService]
})
export class AppComponent {
  constructor(private primengConfig: PrimeNGConfig, private messageService: MessageService) {}

  ngOnInit() {
    this.primengConfig.ripple = true;
  }
}
