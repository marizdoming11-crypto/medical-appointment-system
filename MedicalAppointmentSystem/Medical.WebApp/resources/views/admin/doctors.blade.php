@extends('layouts.app')
@section('content')
<h2>Manage Doctors</h2>
<form class="mb-3"><input class="form-control" name="search" placeholder="Search name or specialization"></form>
<a class="btn btn-success mb-3" href="/admin/doctors/add">Add Doctor</a>
<table class="table table-bordered table-striped"><tr><th>ID</th><th>Doctor</th><th>Specialization</th><th>Contact</th><th>Action</th></tr>
@forelse($doctors ?? [] as $doctor)<tr><td>{{ $doctor['id'] }}</td><td>{{ $doctor['fullName'] }}</td><td>{{ $doctor['specialization'] }}</td><td>{{ $doctor['contactNumber'] }}</td><td><a class="btn btn-sm btn-warning" href="/admin/doctors/edit/{{ $doctor['id'] }}">Edit</a> <a class="btn btn-sm btn-danger" href="/admin/doctors/delete/{{ $doctor['id'] }}">Delete</a></td></tr>@empty<tr><td colspan="5">No doctors found.</td></tr>@endforelse
</table>
@endsection
