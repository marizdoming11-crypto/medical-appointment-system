<!DOCTYPE html>
<html>
<head>
    <title>Medical Appointment System</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body>
<nav class="navbar navbar-expand-lg navbar-dark bg-primary mb-4">
    <div class="container">
        <a class="navbar-brand" href="#">Medical Appointment System</a>
        <div>
            @if(session('role') === 'Admin')
                <a class="btn btn-light btn-sm" href="/admin/dashboard">Dashboard</a>
            @elseif(session('role') === 'User')
                <a class="btn btn-light btn-sm" href="/user/dashboard">Dashboard</a>
            @endif
            @if(session('token'))
                <a class="btn btn-danger btn-sm" href="/logout">Logout</a>
            @endif
        </div>
    </div>
</nav>
<div class="container">
    @if($errors->any())
        <div class="alert alert-danger"><ul class="mb-0">@foreach($errors->all() as $error)<li>{{ $error }}</li>@endforeach</ul></div>
    @endif
    @if(session('success'))<div class="alert alert-success">{{ session('success') }}</div>@endif
    @if(session('error'))<div class="alert alert-danger">{{ session('error') }}</div>@endif
    @yield('content')
</div>
</body>
</html>
