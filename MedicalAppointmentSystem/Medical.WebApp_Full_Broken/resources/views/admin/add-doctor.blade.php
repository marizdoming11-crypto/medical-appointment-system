@extends('layouts.app')
@section('content')
<h2>Add Doctor</h2><form method="POST" action="/admin/doctors/add">@csrf
<div class="mb-3"><label>Name</label><input class="form-control" name="fullName" required></div>
<div class="mb-3"><label>Specialization</label><input class="form-control" name="specialization" required></div>
<div class="mb-3"><label>Contact Number</label><input class="form-control" name="contactNumber" required></div>
<button class="btn btn-success">Save</button> <a class="btn btn-secondary" href="/admin/doctors">Back</a></form>
@endsection
