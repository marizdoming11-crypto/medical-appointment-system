@extends('layouts.app')
@section('content')
<h2>Manage Schedules</h2><a class="btn btn-success mb-3" href="/admin/schedules/add">Add Schedule</a>
<table class="table table-bordered table-striped"><tr><th>ID</th><th>Doctor</th><th>Date</th><th>Time</th><th>Action</th></tr>
@forelse($schedules ?? [] as $s)<tr><td>{{ $s['id'] }}</td><td>{{ $s['doctor']['fullName'] ?? 'No doctor' }}</td><td>{{ $s['scheduleDate'] }}</td><td>{{ $s['startTime'] }} - {{ $s['endTime'] }}</td><td><a class="btn btn-sm btn-danger" href="/admin/schedules/delete/{{ $s['id'] }}">Delete</a></td></tr>@empty<tr><td colspan="5">No schedules found.</td></tr>@endforelse
</table>
@endsection
