<?php
namespace App\Http\Controllers;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class AdminController extends Controller {
    private string $api;
    public function __construct(){ $this->api = env('API_BASE_URL', 'http://127.0.0.1:8001/api'); }
    private function http(){ return Http::withToken(session('token')); }
    public function dashboard(){ return view('admin.dashboard'); }
    public function doctors(){ $doctors = $this->http()->get($this->api.'/Doctors')->json() ?? []; return view('admin.doctors', compact('doctors')); }
    public function addDoctor(){ return view('admin.add-doctor'); }
    public function storeDoctor(Request $r){
        $r->validate(['fullName'=>'required|min:2','specialization'=>'required|min:2','contactNumber'=>'required|min:5']);
        $res = $this->http()->post($this->api.'/Doctors', $r->only(['fullName','specialization','contactNumber']));
        return $res->successful() ? redirect('/admin/doctors')->with('success','Doctor added.') : back()->with('error',$res->json('detail')??$res->body());
    }
    public function editDoctor($id){ $doctor = $this->http()->get($this->api.'/Doctors/'.$id)->json(); return view('admin.edit-doctor', compact('doctor')); }
    public function updateDoctor(Request $r, $id){
        $r->validate(['fullName'=>'required|min:2','specialization'=>'required|min:2','contactNumber'=>'required|min:5']);
        $res = $this->http()->put($this->api.'/Doctors/'.$id, $r->only(['fullName','specialization','contactNumber']));
        return $res->successful() ? redirect('/admin/doctors')->with('success','Doctor updated.') : back()->with('error',$res->json('detail')??$res->body());
    }
    public function deleteDoctor($id){ $this->http()->delete($this->api.'/Doctors/'.$id); return redirect('/admin/doctors')->with('success','Doctor deleted.'); }
    public function schedules(){ $schedules = $this->http()->get($this->api.'/DoctorSchedules')->json() ?? []; return view('admin.schedules', compact('schedules')); }
    public function addSchedule(){ $doctors = $this->http()->get($this->api.'/Doctors')->json() ?? []; return view('admin.add-schedule', compact('doctors')); }
    public function storeSchedule(Request $r){
        $r->validate(['doctorId'=>'required','scheduleDate'=>'required','startTime'=>'required','endTime'=>'required']);
        $res = $this->http()->post($this->api.'/DoctorSchedules', ['doctorId'=>(int)$r->doctorId,'scheduleDate'=>$r->scheduleDate,'startTime'=>$r->startTime,'endTime'=>$r->endTime,'isAvailable'=>true]);
        return $res->successful() ? redirect('/admin/schedules')->with('success','Schedule added.') : back()->with('error',$res->json('detail')??$res->body());
    }
    public function editSchedule($id){ $schedule=$this->http()->get($this->api.'/DoctorSchedules/'.$id)->json(); $doctors=$this->http()->get($this->api.'/Doctors')->json()??[]; return view('admin.edit-schedule', compact('schedule','doctors')); }
    public function updateSchedule(Request $r, $id){
        $r->validate(['doctorId'=>'required','scheduleDate'=>'required','startTime'=>'required','endTime'=>'required']);
        $res = $this->http()->put($this->api.'/DoctorSchedules/'.$id, ['doctorId'=>(int)$r->doctorId,'scheduleDate'=>$r->scheduleDate,'startTime'=>$r->startTime,'endTime'=>$r->endTime,'isAvailable'=>true]);
        return $res->successful() ? redirect('/admin/schedules')->with('success','Schedule updated.') : back()->with('error',$res->json('detail')??$res->body());
    }
    public function deleteSchedule($id){ $this->http()->delete($this->api.'/DoctorSchedules/'.$id); return redirect('/admin/schedules')->with('success','Schedule deleted.'); }
    public function appointments(){ $appointments = $this->http()->get($this->api.'/Appointments')->json() ?? []; return view('admin.appointments', compact('appointments')); }
    public function updateAppointmentStatus($id, $status){
        $res = $this->http()->put($this->api.'/Admin/Appointments/'.$id.'/status/'.$status);
        return $res->successful() ? redirect('/admin/appointments')->with('success','Appointment status updated.') : redirect('/admin/appointments')->with('error',$res->json('detail')??$res->body());
    }
    public function reports(){ $summary = $this->http()->get($this->api.'/Reports/summary')->json() ?? []; return view('admin.reports', compact('summary')); }
}
