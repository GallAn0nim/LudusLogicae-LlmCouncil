import { Outlet } from 'react-router-dom';
import { NavButton } from './NavButton';

export function Layout() {
  return (
    <div className="app-container no-sidebar">
      <NavButton />
      <main className="main-content full-width">
        <Outlet />
      </main>
    </div>
  );
}
