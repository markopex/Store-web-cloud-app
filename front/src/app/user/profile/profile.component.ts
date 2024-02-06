import { HttpEventType } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { MessageService } from 'primeng/api';
import { AuthService } from 'src/app/Shared/services/auth.service';
import { User } from '../shared/user.model';
import { UserService } from '../shared/user.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
  providers: [MessageService]
})
export class ProfileComponent implements OnInit {

  uploadedFile: any;
  profileImg: any = "https://media.istockphoto.com/vectors/default-profile-picture-avatar-photo-placeholder-vector-illustration-vector-id1223671392?k=20&m=1223671392&s=612x612&w=0&h=lGpj2vWAI3WUT1JeJWm1PRoHT3V15_1pdcTn2szdwQ0=";
  user: User;
  role = this.authService.roleStateObservable.value;
  isLoading = false;
  isSavingEdit = false;
  isEditing = false;
  editUserForm = new FormGroup({
    firstName: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(30)]),
    lastName: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(30)]),
    birthdayDate: new FormControl(new Date(), Validators.required),
    birthday: new FormControl(0),
    address: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(50)]),
  });

  constructor(private authService: AuthService, private userService: UserService, private messageService: MessageService, private sanitizer: DomSanitizer) { 
    this.loadUser();
    this.editUserForm.disable();
  }

  ngOnInit(): void {
    this.authService.roleStateObservable.subscribe(
      data => {
        this.role = data;
      }
    )
  }

  updateUser(){
    if(this.editUserForm.valid){
      console.log(this.editUserForm.value);
      //this.userService.
    }
  }

  loadUser(){
    this.userService.getUser().subscribe(
      data => {
        console.log(data);
        this.user = data;
        data.birthdayDate = new Date(data.birthday);
        this.editUserForm.patchValue(data);
        this.userService.downloadFile().subscribe(
          data => {
            if(data.type == HttpEventType.Response){
              const downloadedFile = new Blob([data!.body!], { type: data!.body!.type });
              let objectURL = URL.createObjectURL(downloadedFile);       
              this.profileImg = this.sanitizer.bypassSecurityTrustUrl(objectURL);
            }
          },
          error => {
            console.log(error.error.text);
            //let objectURL = URL.createObjectURL(error.text);       
            //this.image = error.error.text;//this.sanitizer.bypassSecurityTrustUrl(objectURL);          
          }
        )
        //this.editUserForm.value.birthdayDate = new Date(data.birthday).toString();
      }
    )
  }

  saveChanges(){
    if(this.editUserForm.valid){
      this.isSavingEdit = true;
      this.editUserForm.value.birthday = new Date(this.editUserForm.value.birthdayDate).getTime();
      this.userService.updateUser(this.editUserForm.value).subscribe(
        data => {
          this.messageService.add({severity:'success', summary: 'Success', detail: 'Changes saved.'});
          this.isEditing = false;
          this.isSavingEdit = false;
          this.editUserForm.disable();
        },
        error => {
          this.messageService.add({severity:'error', summary: 'Error', detail: error.error.message});
          this.isSavingEdit = false;
        }
      )
    }
  }

  edit(){
    this.editUserForm.enable();
    this.isEditing = true;
  }

  cancelEdit(){
    this.isEditing = false;
    this.editUserForm.disable();
    this.loadUser();
  }

  onUpload(event) {
    const uploadData = new FormData();
    uploadData.append('myFile', this.uploadedFile!, this.uploadedFile?.name);
    this.userService.uploadImage(uploadData).subscribe(
      data => {

        this.messageService.add({severity: 'info', summary: 'File Uploaded', detail: ''});

      },
      error => {
        console.log(error);
      }
    )
}
onSelectImage(event) {
  this.uploadedFile = event.currentFiles[0];
  this.profileImg = this.uploadedFile.objectURL;
}

  apply(){
    this.isLoading = true;
    this.userService.apply().subscribe(
      data => {
        this.isLoading = false;
        this.messageService.add({severity:'success', summary: 'Success', detail: 'Successfully applied. Check email.'});
        this.loadUser();
      },
      error => {
        this.isLoading = false;
        this.messageService.add({severity:'error', summary: 'Error', detail: error.error.message});
      }
    )
  }

}
