import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
baseUrl=environment.apiUrl;
  constructor(private httpclient:HttpClient) { }
  getUsersWithRoles(){
    return this.httpclient.get<User[]>(this.baseUrl+'admin/users-with-roles');
  }
  updateUserRoles(username:string,roles:string[]){
  return this.httpclient.post<string[]>
  (this.baseUrl+'admin/edit-roles/'+username+'?'+roles,{})
  }
}
