@extends('layouts.app')
@section('content')
<div class="row justify-content-center"><div class="col-md-5">
<h2>Login</h2>
<form method="POST" action="/login">@csrf
<div class="mb-3"><label>Username</label><input class="form-control" name="username" required></div>
<div class="mb-3"><label>Password</label><input class="form-control" type="password" name="password" required></div>
<button class="btn btn-primary">Login</button> <a href="/register" class="btn btn-link">Register Patient</a>
</form>
</div></div>
@endsection
