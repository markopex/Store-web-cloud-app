import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { UserService } from '../shared/user.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  providers: [MessageService]
})
export class RegisterComponent implements OnInit {

  isLoading = false;
  registerForm = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.required, Validators.email, Validators.minLength(3), Validators.maxLength(30)]),
    password: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(30)]),
    firstname: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(30)]),
    lastname: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(30)]),
    birthdayDate: new FormControl('', Validators.required),
    birthday: new FormControl(0),
    address: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(50)]),
  });

  constructor(private userService: UserService, private messageService: MessageService, private router: Router) { }

  ngOnInit(): void {
  }

  register(){
    if(this.registerForm.valid){
      this.registerForm.value.birthday = new Date(this.registerForm.value.birthdayDate).getTime();
      console.log(this.registerForm.value);
      this.userService.register(this.registerForm.value).subscribe(
        data =>{
          this.messageService.add({severity:'success', summary: 'Success', detail: 'Login successfull'});
          this.router.navigateByUrl("/login")
        },
        error => {
          this.messageService.add({severity:'error', summary: 'Error', detail: error.error.message});
        }
      )
    }
  }

}
