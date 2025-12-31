import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { FaSearch, FaTrash } from "react-icons/fa";
import "bootstrap/dist/css/bootstrap.min.css";

export default function Appointment() {
  const navigate = useNavigate();

  const [appointments, setAppointments] = useState([
    { time: "9:00 AM", date: "2025-07-10", name: "Chamara", age: 32, doctor: "Dr. Samarasionha" },
    { time: "10:30 AM", date: "2025-09-12", name: "Gayan", age: 28, doctor: "Dr. Chathura" },
    { time: "10:30 AM", date: "2025-04-08", name: "Pasindu", age: 24, doctor: "Dr. Naddep" },
  ]);

  const handleDelete = (index) => {
    setAppointments(appointments.filter((_, i) => i !== index));
  };

  const goToAppointmentBooking = () => {
    navigate("/appoinment"); // Navigate to AppointmentBooking page
  };

  return (
    <div className="container-fluid">
      <div className="row">
        {/* Main Content */}
        <main className="col-md-9 col-lg-10 p-4">
          <h1 className="mb-4">Appointments</h1>

          {/* Tabs */}
          <div className="d-flex mb-3 align-items-center flex-wrap">
            <button className="btn btn-outline-secondary me-2 mb-2">Completed Appointments</button>
            <button className="btn btn-primary mb-2" onClick={goToAppointmentBooking}>
              Go to Appointment Booking
            </button>
          </div>

          {/* Search & Filter */}
          <div className="d-flex mb-4 flex-wrap">
            <div className="input-group me-2 mb-2">
              <span className="input-group-text"><FaSearch /></span>
              <input type="text" className="form-control" placeholder="Search" />
            </div>
            <button className="btn btn-outline-primary mb-2">Filter by Date</button>
          </div>

          {/* Appointment Table */}
          <div className="table-responsive">
            <table className="table table-bordered table-hover align-middle">
              <thead className="table-primary">
                <tr>
                  <th>Time</th>
                  <th>Date</th>
                  <th>Patient Name</th>
                  <th>Patient Age</th>
                  <th>Doctor</th>
                  <th>User Action</th>
                </tr>
              </thead>
              <tbody>
                {appointments.map((appt, idx) => (
                  <tr key={idx}>
                    <td>{appt.time}</td>
                    <td>{appt.date}</td>
                    <td>{appt.name}</td>
                    <td>{appt.age}</td>
                    <td>{appt.doctor}</td>
                    <td className="d-flex align-items-center gap-2">
                      <a href="#" className="btn btn-sm btn-outline-warning">Reschedule</a>
                      <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(idx)}>
                        <FaTrash />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </main>
      </div>
    </div>
  );
}