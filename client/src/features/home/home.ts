import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Register } from '../account/register/register';

@Component({
  selector: 'app-home',
  imports: [Register, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected registerMode = signal(false);

  showRegister(value: boolean) {
    this.registerMode.set(value);
  }
}
