@extends('layouts.app')
@section('content')
<h2>Add Schedule</h2><form method="POST" action="/admin/schedules/add">@csrf
<div class="mb-3"><label>Doctor</label><select class="form-control" name="doctorId" required>@foreach($doctors ?? [] as $doctor)<option value="{{ $doctor['id'] }}">{{ $doctor['fullName'] }} - {{ $doctor['specialization'] }}</option>@endforeach</select></div>
<div class="mb-3"><label>Date</label><input class="form-control" type="date" name="scheduleDate" required></div>
<div class="mb-3"><label>Display Start</label><input class="form-control" name="startTime" placeholder="08:00 AM" required></div>
<div class="mb-3"><label>Display End</label><input class="form-control" name="endTime" placeholder="05:00 PM" required></div>
<div class="mb-3"><label>Start Time 24h</label><input class="form-control" type="time" step="1" name="startTime24" required></div>
<div class="mb-3"><label>End Time 24h</label><input class="form-control" type="time" step="1" name="endTime24" required></div>
<button class="btn btn-success">Save</button> <a class="btn btn-secondary" href="/admin/schedules">Back</a></form>
@endsection
