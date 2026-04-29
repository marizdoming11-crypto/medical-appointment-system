<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\AuthController;
use App\Http\Controllers\AdminController;
use App\Http\Controllers\UserController;

Route::get('/', [AuthController::class, 'login']);
Route::get('/login', [AuthController::class, 'login']);
Route::post('/login', [AuthController::class, 'loginPost']);
Route::get('/register', [AuthController::class, 'register']);
Route::post('/register', [AuthController::class, 'registerPost']);
Route::get('/logout', [AuthController::class, 'logout']);

Route::get('/admin/dashboard', [AdminController::class, 'dashboard']);
Route::get('/admin/doctors', [AdminController::class, 'doctors']);
Route::get('/admin/doctors/add', [AdminController::class, 'addDoctor']);
Route::post('/admin/doctors/add', [AdminController::class, 'storeDoctor']);
Route::get('/admin/doctors/edit/{id}', [AdminController::class, 'editDoctor']);
Route::post('/admin/doctors/edit/{id}', [AdminController::class, 'updateDoctor']);
Route::get('/admin/doctors/delete/{id}', [AdminController::class, 'deleteDoctor']);
Route::get('/admin/schedules', [AdminController::class, 'schedules']);
Route::get('/admin/schedules/add', [AdminController::class, 'addSchedule']);
Route::post('/admin/schedules/add', [AdminController::class, 'storeSchedule']);
Route::get('/admin/schedules/delete/{id}', [AdminController::class, 'deleteSchedule']);
Route::get('/admin/appointments', [AdminController::class, 'appointments']);

Route::get('/user/dashboard', [UserController::class, 'dashboard']);
Route::get('/user/doctors', [UserController::class, 'doctors']);
Route::get('/user/appointments', [UserController::class, 'appointments']);
Route::get('/user/appointments/create', [UserController::class, 'createAppointment']);
Route::post('/user/appointments/create', [UserController::class, 'storeAppointment']);
Route::get('/user/appointments/cancel/{id}', [UserController::class, 'cancelAppointment']);
