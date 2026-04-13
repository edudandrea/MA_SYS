import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule, RouterOutlet } from '@angular/router';

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

  constructor(private router: Router) {}

  ngOnInit() {

    const usuario = JSON.parse(localStorage.getItem('usuario')|| '{}');

    this.userName = usuario.userName
    this.academiaNome = usuario.academiaNome;

    console.log('🧠 Usuário Logado:', usuario);
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

  logout(){
    localStorage.clear();
    window.location.href = '/login';

  }

}
