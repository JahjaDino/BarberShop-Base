import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import MainFrame from './components/layout/MainFrame'
import BookAppointmentPage from './features/appointments/BookAppointmentPage'
import MyAppointmentsPage from './features/appointments/MyAppointmentsPage'
import RoleDashboard from './features/RoleDashboard'
import ProfilePage from './features/client/ProfilePage'
import EmployeeAppointmentsPage from './features/employee/EmployeeAppointmentsPage'
import EmployeeDashboard from './features/employee/EmployeeDashboard'
import EmployeeProfilePage from './features/employee/EmployeeProfilePage'
import EmployeeSchedulePage from './features/employee/EmployeeSchedulePage'
import EmployeeServicesPage from './features/employee/EmployeeServicesPage'
import EmployeeTimeOffPage from './features/employee/EmployeeTimeOffPage'
import OwnerAppointmentsPage from './features/owner/OwnerAppointmentsPage'
import OwnerDashboard from './features/owner/OwnerDashboard'
import OwnerEmployeesPage from './features/owner/OwnerEmployeesPage'
import OwnerServicesPage from './features/owner/OwnerServicesPage'
import OwnerSettingsPage from './features/owner/OwnerSettingsPage'
import OwnerTimeOffRequestsPage from './features/owner/OwnerTimeOffRequestsPage'
import ServicesPage from './features/services/ServicesPage'
import HomePage from './pages/HomePage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/app" element={<MainFrame />}>
          <Route index element={<Navigate to="/app/dashboard" replace />} />
          <Route path="dashboard" element={<RoleDashboard />} />
          <Route path="book-appointment" element={<BookAppointmentPage />} />
          <Route path="my-appointments" element={<MyAppointmentsPage />} />
          <Route path="services" element={<ServicesPage />} />
          <Route path="profile" element={<ProfilePage />} />

          <Route path="owner/dashboard" element={<OwnerDashboard />} />
          <Route path="owner/appointments" element={<OwnerAppointmentsPage />} />
          <Route path="owner/employees" element={<OwnerEmployeesPage />} />
          <Route path="owner/time-off" element={<OwnerTimeOffRequestsPage />} />
          <Route path="owner/services" element={<OwnerServicesPage />} />
          <Route path="owner/settings" element={<OwnerSettingsPage />} />

          <Route path="employee/dashboard" element={<EmployeeDashboard />} />
          <Route path="employee/schedule" element={<EmployeeSchedulePage />} />
          <Route path="employee/appointments" element={<EmployeeAppointmentsPage />} />
          <Route path="employee/time-off" element={<EmployeeTimeOffPage />} />
          <Route path="employee/services" element={<EmployeeServicesPage />} />
          <Route path="employee/profile" element={<EmployeeProfilePage />} />
        </Route>

        <Route
          path="/dashboard"
          element={<Navigate to="/app/dashboard" replace />}
        />
      </Routes>
    </BrowserRouter>
  )
}

export default App