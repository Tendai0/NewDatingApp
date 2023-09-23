import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'DatingApp';
  users:any;
  constructor(private accountService:AccountService){}
  ngOnInit(): void {
    this.SetCurrentUser();
  } 
  SetCurrentUser(){
    const Userstring = localStorage.getItem('user');
    if(!Userstring) return;
    const user : User = JSON.parse(Userstring);
    this.accountService.setCurrentUser(user);

  }
}
