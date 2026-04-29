@extends('layouts.app')
@section('content')
<h2>Create Appointment</h2><form method="POST" action="/user/appointments/create">@csrf
<div class="mb-3"><label>Doctor</label><select class="form-control" name="doctorId" required>@foreach($doctors ?? [] as $doctor)<option value="{{ $doctor['id'] }}">{{ $doctor['fullName'] }} - {{ $doctor['specialization'] }}</option>@endforeach</select></div>
<div class="mb-3"><label>Date</label><input class="form-control" type="date" name="appointmentDate" required></div>
<div class="mb-3"><label>Time</label><input class="form-control" type="time" step="1" name="appointmentTime" required></div>
<div class="mb-3"><label>Reason</label><input class="form-control" name="reason" required></div>
<button class="btn btn-success">Save Appointment</button> <a class="btn btn-secondary" href="/user/dashboard">Back</a>
</form>
@endsection
