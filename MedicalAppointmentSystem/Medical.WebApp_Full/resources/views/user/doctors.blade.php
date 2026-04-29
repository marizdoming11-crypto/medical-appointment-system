@extends('layouts.app')
@section('content')
<h2>Available Doctors</h2>
<table class="table table-bordered table-striped"><tr><th>ID</th><th>Doctor</th><th>Specialization</th><th>Contact</th></tr>
@forelse($doctors ?? [] as $doctor)<tr><td>{{ $doctor['id'] }}</td><td>{{ $doctor['fullName'] }}</td><td>{{ $doctor['specialization'] }}</td><td>{{ $doctor['contactNumber'] }}</td></tr>@empty<tr><td colspan="4">No doctors found.</td></tr>@endforelse
</table>
@endsection
