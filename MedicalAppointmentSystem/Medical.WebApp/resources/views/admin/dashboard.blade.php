@extends('layouts.app')
@section('content')
<h2>Admin Dashboard</h2><p>Welcome, {{ session('full_name') }}</p>
<div class="row">
<div class="col-md-4"><a class="btn btn-primary w-100 mb-2" href="/admin/doctors">Manage Doctors</a></div>
<div class="col-md-4"><a class="btn btn-primary w-100 mb-2" href="/admin/schedules">Manage Schedules</a></div>
<div class="col-md-4"><a class="btn btn-primary w-100 mb-2" href="/admin/appointments">View Appointments</a></div>
</div>
@endsection
