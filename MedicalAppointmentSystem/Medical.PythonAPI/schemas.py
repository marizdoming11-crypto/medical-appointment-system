from datetime import date, time
from typing import Optional
from pydantic import BaseModel, Field


class RegisterDto(BaseModel):
    fullName: str = Field(min_length=2)
    username: str = Field(min_length=3)
    password: str = Field(min_length=6)


class LoginDto(BaseModel):
    username: str = Field(min_length=3)
    password: str = Field(min_length=6)


class TokenResponse(BaseModel):
    access_token: str
    token_type: str
    id: int
    fullName: str
    username: str
    role: str


class UserResponse(BaseModel):
    id: int
    fullName: str
    username: str
    role: str

    model_config = {"from_attributes": True}


class DoctorCreate(BaseModel):
    fullName: str = Field(min_length=2)
    specialization: str = Field(min_length=2)
    contactNumber: str = Field(min_length=5)


class DoctorResponse(DoctorCreate):
    id: int

    model_config = {"from_attributes": True}


class ScheduleCreate(BaseModel):
    doctorId: int
    scheduleDate: date
    startTime: str
    endTime: str
    startTime24: time
    endTime24: time
    isAvailable: bool = True


class ScheduleResponse(ScheduleCreate):
    id: int
    doctor: Optional[DoctorResponse] = None

    model_config = {"from_attributes": True}


class AppointmentCreate(BaseModel):
    userId: int
    doctorId: int
    appointmentDate: date
    appointmentTime: time
    reason: str = Field(min_length=2)
    status: str = "Pending"


class AppointmentUpdate(BaseModel):
    doctorId: int
    appointmentDate: date
    appointmentTime: time
    reason: str = Field(min_length=2)


class AppointmentResponse(AppointmentCreate):
    id: int
    user: Optional[UserResponse] = None
    doctor: Optional[DoctorResponse] = None

    model_config = {"from_attributes": True}
