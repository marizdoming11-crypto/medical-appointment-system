@extends('layouts.app')
@section('content')
<div class="row justify-content-center"><div class="col-md-5">
<h2>Register Patient</h2>
<form method="POST" action="/register">@csrf
<div class="mb-3"><label>Full Name</label><input class="form-control" name="fullName" required></div>
<div class="mb-3"><label>Username</label><input class="form-control" name="username" required></div>
<div class="mb-3"><label>Password</label><input class="form-control" type="password" name="password" required></div>
<button class="btn btn-success">Register</button> <a href="/login" class="btn btn-secondary">Back</a>
</form>
</div></div>
@endsection
