import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from "@angular/router";
import { ThemeService } from '../Services/Theme/theme.service';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { AuthService } from '../Services/Auth/Auth.service';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule, NgxSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App implements OnInit, OnDestroy {
  private spinnerSubscription?: Subscription;
  private spinnerWasVisible = false;
  readonly martialSpinners = ['boxing-glove', 'muay-thai-prajied', 'jiu-jitsu-belt'] as const;
  currentMartialSpinner: (typeof this.martialSpinners)[number] = 'boxing-glove';

  constructor(
    private themeService: ThemeService,
    private auth: AuthService,
    private spinner: NgxSpinnerService
  ) {}

  ngOnInit(): void {
    this.themeService.initializeTheme();
    this.auth.initializeIdleTimeout();
    this.spinnerSubscription = this.spinner.spinnerObservable.subscribe((spinner) => {
      if (spinner.show && !this.spinnerWasVisible) {
        this.randomizeMartialSpinner();
      }

      this.spinnerWasVisible = spinner.show;
    });
  }

  ngOnDestroy(): void {
    this.spinnerSubscription?.unsubscribe();
  }

  private randomizeMartialSpinner(): void {
    const nextIndex = Math.floor(Math.random() * this.martialSpinners.length);
    this.currentMartialSpinner = this.martialSpinners[nextIndex];
  }

  protected readonly title = signal('UNiFlow Dojo'); 
 
}
