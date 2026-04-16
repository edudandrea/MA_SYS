import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';

@Component({
  selector: 'app-Layout',
  templateUrl: './Layout.component.html',
  styleUrls: ['./Layout.component.css'],
  imports: [CommonModule, RouterOutlet, RouterModule],
})
export class LayoutComponent implements OnInit {
  sidebarCollapsed = false;
  userName: string = '';
  academiaNome: string = '';
  usuario: any;
  plataformId = inject(PLATFORM_ID);
  financeiroOpen = false;
  cadastroOpen = false;

  constructor(private router: Router) {}

  ngOnInit() {
    if (isPlatformBrowser(this.plataformId)) {
      this.getUserInfo();
    }
  }

  toggleFinanceiro() {
    this.financeiroOpen = !this.financeiroOpen;
  }

  toggleCadastro() {
    this.cadastroOpen = !this.cadastroOpen;
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  onLogout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  isAdmin(): boolean {
    const role = localStorage.getItem('role');
    return role === 'Admin';
  }

  logout() {
    localStorage.clear();
    window.location.href = '/login';
  }

  getUserInfo() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');

    this.userName = usuario.userName;
    this.academiaNome = usuario.academiaNome;

    console.log('🧠 Usuário Logado:', usuario);
  }
}
