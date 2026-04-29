@extends('layouts.app')
@section('content')
<h2>All Appointments</h2><table class="table table-bordered table-striped"><tr><th>ID</th><th>Patient</th><th>Doctor</th><th>Date</th><th>Time</th><th>Reason</th><th>Status</th></tr>
@forelse($appointments ?? [] as $a)<tr><td>{{ $a['id'] }}</td><td>{{ $a['user']['fullName'] ?? 'No patient' }}</td><td>{{ $a['doctor']['fullName'] ?? 'No doctor' }}</td><td>{{ $a['appointmentDate'] }}</td><td>{{ $a['appointmentTime'] }}</td><td>{{ $a['reason'] }}</td><td>{{ $a['status'] }}</td></tr>@empty<tr><td colspan="7">No appointments found.</td></tr>@endforelse
</table>
@endsection
