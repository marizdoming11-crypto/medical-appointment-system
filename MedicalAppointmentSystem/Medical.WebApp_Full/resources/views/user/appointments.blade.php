@extends('layouts.app')
@section('content')
<h2>My Appointments</h2><a class="btn btn-success mb-3" href="/user/appointments/create">Make Appointment</a>
<table class="table table-bordered table-striped"><tr><th>ID</th><th>Doctor</th><th>Date</th><th>Time</th><th>Reason</th><th>Status</th><th>Action</th></tr>
@forelse($appointments ?? [] as $a)<tr><td>{{ $a['id'] }}</td><td>{{ $a['doctor']['fullName'] ?? 'No doctor' }}</td><td>{{ $a['appointmentDate'] }}</td><td>{{ $a['appointmentTime'] }}</td><td>{{ $a['reason'] }}</td><td>{{ $a['status'] }}</td><td>@if($a['status'] !== 'Cancelled')<a class="btn btn-sm btn-danger" href="/user/appointments/cancel/{{ $a['id'] }}">Cancel</a>@endif</td></tr>@empty<tr><td colspan="7">No appointments found.</td></tr>@endforelse
</table>
@endsection
