import { Component, OnInit } from '@angular/core';
import { Router, RouterEvent } from '@angular/router';
import { AuthService } from '../../Shared/services/auth.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {

  isLoggedIn = this.authService.authStateObservable.value;
  role = this.authService.roleStateObservable.value;

  constructor(private router: Router, private authService: AuthService) { }

  ngOnInit(): void {
    this.authService.authStateObservable.subscribe(
      value => {
        this.isLoggedIn = value;
      }
    );
    this.authService.roleStateObservable.subscribe(
      value => {
        this.role = value; 
      }
    )
  }

  logout(){
    this.authService.logoutUser();
    this.router.navigateByUrl('/login');
  }

}
