import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-signup',
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    ButtonModule
  ],
  templateUrl: './signup.html',
  styleUrl: './signup.scss'
})
export class SignupComponent {}
