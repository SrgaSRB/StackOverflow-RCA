import './App.css';
import { createBrowserRouter, RouterProvider, Navigate, Outlet } from 'react-router-dom';
import Register from './pages/Register';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Post from './pages/Post';
import Profile from './pages/Profile';
import Post2 from './pages/Post2';
import AdminHealth from './pages/AdminHealth';
import HealthLive from './pages/HealthLive';
import HealthHistory from './pages/HealthHistory';
import NotFound from './components/NotFound';
import Navbar from './components/navbar';
import Loader from './components/Loader';
import Notification from './components/Notification';

// Layout sa Navbar-om za sve osim login/register
const RootLayout = () => (
  <div className="App">
    <Navbar />
    {/* <Loader /> */}
    {/* <Notification /> */}
    <div style={{ padding: '2rem' }}>
      <Outlet />
    </div>
  </div>
);

// Layout bez Navbar-a za login/register
const PlainLayout = () => (
  <div>
    <Outlet />
  </div>
);

const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/user-login" replace />
  },
  {
    element: <PlainLayout />,
    children: [
      { path: '/user-login', element: <Login /> },
      { path: '/user-register', element: <Register /> }
    ]
  },
  {
    element: <RootLayout />,
    children: [
      { path: '/dashboard', element: <Dashboard /> },
      { path: '/post/:postId', element: <Post /> },
      { path: '/profile', element: <Profile /> },
      { path: '/profile/:id', element: <Profile /> },
      { path: '/create-post', element: <Post2 /> },
      { path: '/update-post/:id', element: <Post2 /> },
      { path: '/admin/health', element: <AdminHealth /> },
      { path: '/health/live', element: <HealthLive /> },
      { path: '/health/history', element: <HealthHistory /> },
      { path: '*', element: <NotFound /> }
    ]
  }
]);

const App = () => <RouterProvider router={router} />;

export default App;