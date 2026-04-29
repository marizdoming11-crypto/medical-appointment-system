@extends('layouts.app')
@section('content')
<h2>User Dashboard</h2><p>Welcome, {{ session('full_name') }}</p>
<div class="row">
<div class="col-md-4"><a class="btn btn-primary w-100 mb-2" href="/user/doctors">View Doctors</a></div>
<div class="col-md-4"><a class="btn btn-success w-100 mb-2" href="/user/appointments/create">Make Appointment</a></div>
<div class="col-md-4"><a class="btn btn-info w-100 mb-2" href="/user/appointments">My Appointments</a></div>
</div>
@endsection
