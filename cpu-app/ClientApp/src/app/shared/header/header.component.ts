import { Component, OnInit, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { StateService } from '../../core/services/state.service';
import { nameAssemble } from '../../core/constants/name-assemble'
import { UserDataService } from '../../core/services/user-data.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  title: string = 'Victim Services Community Programs Unit';
  currentUser: string;
  nameAssemble;
  loggedIn: boolean = false;
  loading = false;
  window = window;
  isNewUserRegistration: boolean = false;
  apiPath = environment.apiRootUrl;
  constructor(
    private router: Router,
    private stateService: StateService,
    private userData: UserDataService,
  ) {
    // for building names
    this.nameAssemble = nameAssemble;
  }

  ngOnInit() {
    this.stateService.loggedIn.subscribe((l: boolean) => {
      this.loggedIn = l;
      if (window.location.href.indexOf("login") < 0) {
        this.router.navigate([this.stateService.homeRoute.getValue()]);
      }
    });
    this.stateService.currentUser.subscribe(u => {
      if (u) {
        this.currentUser = nameAssemble(u.firstName, u.middleName, u.lastName);
      }
    });
    this.stateService.loading.subscribe(l => this.loading = l);
  }
  login() {
    if (window.location.href.includes("localhost")) {
      this.stateService.login();
    }
    else {
      window.location.href = 'login';
    }
  }
  logout() {
    this.stateService.logout();
  }
  homeButton() {
    // this is done without a routerlink because you will want to route the user back to a place
    // that is appropriate for their role. So check their logged in state and etc before deciding which route they go to.
    this.router.navigate([this.stateService.homeRoute.getValue()]);
  }
}
